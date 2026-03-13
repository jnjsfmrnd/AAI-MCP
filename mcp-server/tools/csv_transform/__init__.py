import json
import logging
import pandas as pd
import azure.functions as func


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("csv_transform tool invoked")

    try:
        body = req.get_json()
    except ValueError:
        return func.HttpResponse(
            json.dumps({"tool": "csv.transform", "success": False, "error": "Invalid JSON"}),
            status_code=400,
            mimetype="application/json",
        )

    csv_text = body.get("csv")
    instructions = body.get("instructions", "")

    if csv_text is None:
        return func.HttpResponse(
            json.dumps({"tool": "csv.transform", "success": False, "error": "Missing 'csv'"}),
            status_code=400,
            mimetype="application/json",
        )

    try:
        df = pd.read_csv(pd.io.common.StringIO(csv_text))

        if "remove empty" in instructions.lower():
            df = df.dropna(how="all")

        output_csv = df.to_csv(index=False)

        return func.HttpResponse(
            json.dumps({"tool": "csv.transform", "success": True, "data": output_csv}),
            status_code=200,
            mimetype="application/json",
        )
    except Exception as exc:
        logging.exception("Failed to transform CSV")
        return func.HttpResponse(
            json.dumps({"tool": "csv.transform", "success": False, "error": str(exc)}),
            status_code=500,
            mimetype="application/json",
        )
