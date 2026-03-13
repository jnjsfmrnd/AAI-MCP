import json
import logging
import azure.functions as func
from azure.storage.blob import ContainerClient


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("blob_list tool invoked")

    try:
        body = req.get_json()
    except ValueError:
        return func.HttpResponse(
            json.dumps({"tool": "blob.list", "success": False, "error": "Invalid JSON"}),
            status_code=400,
            mimetype="application/json",
        )

    container = body.get("container")
    if not container:
        return func.HttpResponse(
            json.dumps({"tool": "blob.list", "success": False, "error": "Missing 'container'"}),
            status_code=400,
            mimetype="application/json",
        )

    connection_string = _get_connection_string()
    if not connection_string:
        return func.HttpResponse(
            json.dumps({"tool": "blob.list", "success": False, "error": "Azure storage connection string not configured"}),
            status_code=500,
            mimetype="application/json",
        )

    try:
        container_client = ContainerClient.from_connection_string(connection_string, container_name=container)
        blobs = [b.name for b in container_client.list_blobs()]

        return func.HttpResponse(
            json.dumps({"tool": "blob.list", "success": True, "data": blobs}),
            status_code=200,
            mimetype="application/json",
        )
    except Exception as exc:
        logging.exception("Failed to list blobs")
        return func.HttpResponse(
            json.dumps({"tool": "blob.list", "success": False, "error": str(exc)}),
            status_code=500,
            mimetype="application/json",
        )


def _get_connection_string() -> str:
    # When deployed, set AZURE_STORAGE_CONNECTION_STRING in Function App configuration.
    # For local development, set it in local.settings.json under Values.
    import os

    return os.getenv("AZURE_STORAGE_CONNECTION_STRING")
