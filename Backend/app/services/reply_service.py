"""
Persian:
    ????? ????? ???? ?? ?????.
English:
    Service for sending replies to comments.

Install:
    pip install instagrapi
"""

class ReplyService:
    """
    Persian:
        ????? ????? ???? ?? ?????.
    English:
        Service for sending replies to comments.
    """
    async def send_reply(self, client, media_id, comment, rule):
        # Send reply text, link, image, or share post as per rule
        # TODO: Implement random reply selection and media/link support
        await client.comment_reply(media_id, comment.id, rule.get_random_reply())
