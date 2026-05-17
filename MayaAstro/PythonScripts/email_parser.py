import sys
import json

def parse_email(email_content):
    # In a real case, you'd parse the email content.
    # For simplicity, we'll extract a few values manually.
    parsed_data = {
        "subject": "Test Subject from Python",
        "sender": "sender@example.com",
        "body": email_content
    }
    return parsed_data

if __name__ == "__main__":
    email_content = sys.argv[1]  # Get email content from command line argument
    parsed_data = parse_email(email_content)
    print(json.dumps(parsed_data)) 
