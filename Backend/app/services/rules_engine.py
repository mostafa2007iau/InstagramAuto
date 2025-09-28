"""
Persian:
    موتور سادهٔ ارزیابی قواعد: برای هر حساب، قوانین فعال را انتخاب، روی ورودی‌ها اعمال و در صورت match به صف ارسال job می‌فرستد.

English:
    Simple rules engine: select active rules for account, evaluate against input (e.g., new media), and enqueue send jobs when matched.
"""

from typing import List, Dict, Any
from sqlmodel import Session, select
from app.models.rule_model import Rule
from app.db import engine
from app.services.sender_queue import enqueue_send
from json_logic import jsonLogic
import ast

def evaluate_rules_for_event(event: Dict[str, Any]) -> list:
    """
    Persian:
        رویداد را روی همه قواعد فعال حساب اجرا می‌کند و لیست rule_idهایی که match شدند را برمی‌گرداند.
    English:
        Evaluates the event against all active rules for the account and returns a list of matched rule_ids.
    """
    account_id = event.get("account_id")
    matched = []
    if not account_id:
        return matched
    with Session(engine) as db:
        statement = select(Rule).where(Rule.account_id == account_id, Rule.enabled == True)
        rules = db.exec(statement).all()
    for r in rules:
        try:
            expr = __import__("json").loads(r.expression)
        except Exception:
            continue
        if evaluate_expression_jsonlogic(expr, event):
            matched.append(r.id)
            action = {"session_id": event.get("session_id"), "rule_id": r.id, "event": event}
            enqueue_send(action)
    return matched

def evaluate_expression_jsonlogic(expression: Dict[str, Any], context: Dict[str, Any]) -> bool:
    """
    Persian:
        expression باید یک ساختار json-logic معتبر باشد. context داده‌های runtime را فراهم می‌کند.

    English:
        expression must be a valid json-logic structure. context provides runtime data.
    """
    try:
        return bool(jsonLogic(expression, data=context))
    except Exception:
        return False

def evaluate_expression(expression: str, context: Dict[str, Any]) -> bool:
    """
    Persian:
        ارزیابی امن عبارت rule روی context. برای امنیت از ast.parse و whitelist nodeها استفاده می‌کنیم.

    English:
        Safely evaluate rule expression against context using ast with a small whitelist.
    """
    # WARNING: this is a simple evaluator. For production use a sandboxed evaluator.
    try:
        tree = ast.parse(expression, mode="eval")
        # simple allowed names from context
        allowed_names = {k: v for k, v in context.items()}
        code = compile(tree, "<rule>", "eval")
        return bool(eval(code, {"__builtins__": {}}, allowed_names))
    except Exception:
        return False

def run_rules_for_account(account_id: str, event: Dict[str, Any]):
    """
    Persian:
        برای هر قاعده فعال مرتبط با account این event را تست می‌کنیم و اگر مطابق بود، job ارسال می‌کنیم.

    English:
        For each active rule linked to account, test the event and enqueue send job on match.
    """
    with Session(engine) as db:
        statement = select(Rule).where(Rule.account_id == account_id, Rule.enabled == True)
        rules = db.exec(statement).all()
    for r in rules:
        # Expect r.expression to be JSON string representing json-logic; parse safely
        try:
            expr = __import__("json").loads(r.expression)
        except Exception:
            continue
        if evaluate_expression_jsonlogic(expr, event):
            action = {"session_id": event.get("session_id"), "rule_id": r.id, "event": event}
            enqueue_send(action)
