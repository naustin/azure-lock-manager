
# Get the blob URL
data "azurerm_storage_blob" "example" {
  name                   = "your-blob-name.json"
  storage_account_name    = data.azurerm_storage_account.example.name
  storage_container_name  = "your-container-name"
}

# Use the HTTP provider to read the blob content from the URL
data "http" "json_content" {
  url = data.azurerm_storage_blob.example.url
}

# Load the JSON content into a local variable
locals {
  json_data = jsondecode(data.http.json_content.body)
}

# Output to verify the loaded JSON content
output "json_content" {
  value = local.json_data
}
