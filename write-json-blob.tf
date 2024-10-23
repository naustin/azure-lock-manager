provider "azurerm" {
  features {}
}

# Resource group
resource "azurerm_resource_group" "example" {
  name     = "example-resources"
  location = "East US"
}

# Storage account
resource "azurerm_storage_account" "example" {
  name                     = "examplestorageacc"
  resource_group_name      = azurerm_resource_group.example.name
  location                 = azurerm_resource_group.example.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Storage container
resource "azurerm_storage_container" "example" {
  name                  = "contentcontainer"
  storage_account_name  = azurerm_storage_account.example.name
  container_access_type = "private"
}

# A local array variable
variable "my_array" {
  type = list(string)
  default = ["item1", "item2", "item3"]
}

# Manipulate the array (adding a new item, for example)
locals {
  modified_array = concat(var.my_array, ["new_item"])
}

# Azure storage blob with content directly from modified array
resource "azurerm_storage_blob" "example" {
  name                   = "myblob.txt"
  storage_account_name    = azurerm_storage_account.example.name
  storage_container_name  = azurerm_storage_container.example.name
  type                   = "Block"
  
  # Convert the array into a string and write as blob content
  content                = join(",", local.modified_array)
}
