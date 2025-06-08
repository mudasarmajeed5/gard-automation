from flask import Blueprint, jsonify, request
from __init__db import connectDatabase

campaigns_bp = Blueprint("campaigns",__name__,url_prefix="/campaigns")

@campaigns_bp.route("/<int:admin_id>", methods=["GET"])
def get_campaigns(admin_id):
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)
    cursor.execute("SELECT * FROM EmailCampaigns WHERE admin_id = %s", (admin_id,))
    data = cursor.fetchall()
    conn.close()
    return jsonify(data if data else [])


@campaigns_bp.route("/add", methods=["POST"])
def add_campaign():
    try:
        data = request.get_json()
        campaign_name = data.get("name")
        admin_id = data.get("admin_id")
        campaign_content = data.get("content")
        if not campaign_name or not campaign_content or not admin_id:
            return jsonify({"error": "Campaign name, content, and admin_id are required"}), 400

        conn = connectDatabase()
        cursor = conn.cursor()

        query = "INSERT INTO EmailCampaigns (admin_id, campaign_name, content) VALUES (%s, %s, %s)"
        values = (admin_id, campaign_name, campaign_content) 

        cursor.execute(query, values)
        conn.commit()
        conn.close()

        return jsonify({"message": "Campaign added successfully"}), 201

    except Exception as e:
        print("INSERT ERROR:", e)
        return jsonify({"error": "Failed to add campaign"}), 500


@campaigns_bp.route("/delete/<int:id>", methods=["DELETE"])
def delete_campaign(id):
    conn = connectDatabase()
    cursor = conn.cursor()
    cursor.execute("DELETE FROM EmailCampaigns WHERE id = %s", (id,))

    if cursor.rowcount == 0:
        conn.close()
        return jsonify({"error": "Campaign not found"}), 404

    conn.commit()
    conn.close()
    return jsonify({"message": "Campaign deleted successfully!"})
