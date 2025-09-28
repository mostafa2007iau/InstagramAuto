"""
Persian:
    ???? ???? ????? ???? ??? ???????.
English:
    Sample test data for integration tests.
"""
SAMPLE_USERS = [
    {"username": "testuser1", "password": "testpass1"},
    {"username": "testuser2", "password": "testpass2"}
]

SAMPLE_RULES = [
    {
        "account_id": "testuser1",
        "media_id": None,
        "condition": '{"==": [{"var": "text"}, "hello"]}',
        "replies": ["Hi!", "Hello!"],
        "send_dm": True,
        "dms": ["Thanks for your comment!"],
        "enabled": True
    }
]
