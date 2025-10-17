import logging
from app.i18n import translate

def test_translate_missing_kwargs_logs_warning(caplog):
    """
    Tests that a warning is logged when a translation key with a placeholder
    is used without providing the necessary keyword arguments.
    """
    key = "proxy.test.failure"
    unformatted_translation = "Proxy test failed: {reason}"

    with caplog.at_level(logging.WARNING):
        result = translate(key, "en")
        assert result == unformatted_translation, "The unformatted string should be returned on failure."
        assert "Failed to format translation" in caplog.text, "A warning message should be logged."
        assert key in caplog.text, "The log message should include the translation key."