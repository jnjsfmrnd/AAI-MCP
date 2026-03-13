import json
import logging
import azure.functions as func
from azure.storage.blob import ContainerClient


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("blob_read tool invoked")

    try:
        body = req.get_json()
    except ValueError:
        return func.HttpResponse(
            json.dumps({"tool": "blob.read", "success": False, "error": "Invalid JSON"}),
            status_code=400,
            mimetype="application/json",
        )

    container = body.get("container")
    blob_name = body.get("blob")
    if not container or not blob_name:
        return func.HttpResponse(
            json.dumps({"tool": "blob.read", "success": False, "error": "Missing 'container' or 'blob'"}),
            status_code=400,
            mimetype="application/json",
        )

    connection_string = _get_connection_string()
    if not connection_string:
        return func.HttpResponse(
            json.dumps({"tool": "blob.read", "success": False, "error": "Azure storage connection string not configured"}),
            status_code=500,
            mimetype="application/json",
        )

    try:
        container_client = ContainerClient.from_connection_string(connection_string, container_name=container)
        blob_client = container_client.get_blob_client(blob_name)
        stream = blob_client.download_blob()
        content = stream.content_as_text(encoding="utf-8")

        return func.HttpResponse(
            json.dumps({"tool": "blob.read", "success": True, "data": content}),
            status_code=200,
            mimetype="application/json",
        )
    except Exception as exc:
        logging.exception("Failed to read blob")
        return func.HttpResponse(
            json.dumps({"tool": "blob.read", "success": False, "error": str(exc)}),
            status_code=500,
            mimetype="application/json",
        )


def _get_connection_string() -> str:
    import os

    return os.getenv("AZURE_STORAGE_CONNECTION_STRING")
