# Azure automation webhook Service Manager (SCSM) connector
System Center Service Manager connector and runbook template for Azure Automation leveraging webhooks.

**This project is not fully functional yet**  
**Note: credentials are currently stored in clear text!**


### teaser :-)
<img src="https://raw.githubusercontent.com/Stijnc/azure-automation-webhook-scsm-connector/master/Screenshots/AA_Connector3.PNG" alt="Drawing" style="width: 50px;"/>
## overview
the solution contains the following elements:
* an Azure Automation Connector
* an Azure Automation Webhook class
* an Azure Automation Webhook Activity
* a Service manager workflow using the azure powershell module to check the runbook job status 

## Installation
todo

## Usage
todo

## Remarks
* webhooks are used instead of querying the automation account
* entering webhooks is a manual task
* I repeat: password is stored in clear text, first I need to get things working :-)
* all other things I forgot to mention, but that are important
