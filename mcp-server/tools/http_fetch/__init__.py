import json
import logging
import aiohttp
import azure.functions as func


def _json_or_text(resp):
    try:
        return resp.json()
    except Exception:
        return resp.text


async def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("http_fetch tool invoked")

    try:
        body = req.get_json()
    except ValueError:
        return func.HttpResponse(
            json.dumps({"tool": "http.fetch", "success": False, "error": "Invalid JSON"}),
            status_code=400,
            mimetype="application/json",
        )

    url = body.get("url")
    if not url:
        return func.HttpResponse(
            json.dumps({"tool": "http.fetch", "success": False, "error": "Missing 'url'"}),
            status_code=400,
            mimetype="application/json",
        )

    try:
        async with aiohttp.ClientSession() as session:
            async with session.get(url) as resp:
                content = await _json_or_text(resp)
                return func.HttpResponse(
                    json.dumps({"tool": "http.fetch", "success": True, "data": content}),
                    status_code=200,
                    mimetype="application/json",
                )
    except Exception as exc:
        logging.exception("Failed to fetch URL")
        return func.HttpResponse(
            json.dumps({"tool": "http.fetch", "success": False, "error": str(exc)}),
            status_code=500,
            mimetype="application/json",
        )
