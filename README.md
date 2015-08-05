# Azure automation webhook Service Manager (SCSM) connector
System Center Service Manager connector and runbook template for Azure Automation leveraging webhooks.

** This project is not fully functional yet**
**Note: credentials are currently stored in clear text!**

## overview
the solution contains the following elements:
* an Azure Automation Connector
* an Azure Automation Webhook class
* an Azure Automation Webhook Activity
* a Service manager workflow using the azure powershell module to check the runbook job status 
