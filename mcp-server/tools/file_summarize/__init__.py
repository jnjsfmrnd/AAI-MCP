import json
import logging
import azure.functions as func


def _summarize(text: str, max_chars: int = 400) -> str:
    if not text:
        return ""

    if len(text) <= max_chars:
        return text

    # Simple summary: return first chunk + ellipsis.
    return text[:max_chars].rstrip() + "..."


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("file_summarize tool invoked")

    try:
        body = req.get_json()
    except ValueError:
        return func.HttpResponse(
            json.dumps({"tool": "file.summarize", "success": False, "error": "Invalid JSON"}),
            status_code=400,
            mimetype="application/json",
        )

    content = body.get("content")
    if content is None:
        return func.HttpResponse(
            json.dumps({"tool": "file.summarize", "success": False, "error": "Missing 'content'"}),
            status_code=400,
            mimetype="application/json",
        )

    try:
        summary = _summarize(str(content))
        return func.HttpResponse(
            json.dumps({"tool": "file.summarize", "success": True, "data": summary}),
            status_code=200,
            mimetype="application/json",
        )
    except Exception as exc:
        logging.exception("Failed to summarize content")
        return func.HttpResponse(
            json.dumps({"tool": "file.summarize", "success": False, "error": str(exc)}),
            status_code=500,
            mimetype="application/json",
        )
