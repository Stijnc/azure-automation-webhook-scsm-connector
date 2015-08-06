<#
    Get the Azure Automation Webhook Runbook Activity
    1. get the class
    2. get the instance
    3. get the webhook relationship
#>

<#
    Get the Azure Automation Webhook
    1. get the class
    2. get the instance
    3. get the connector relationship
#>


<# Get the settings
    1. get the connector class
    2. get connector instance
    3. get the account
    4. split the account in username - password
#>

<#
    trigger the webhook
    1. invoke the url
    2. write the jobid to the Webhook activity
    3. add log item (webhook invoked)

#>

<#
    loop
    1. create credential
    2. authenticate against azure
    3. Check status
    4. timeout or get the details
    5. write details\ output to activity
    6. set activity status (completed or failed)
#>