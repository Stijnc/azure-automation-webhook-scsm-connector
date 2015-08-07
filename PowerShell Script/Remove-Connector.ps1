Get-SCSMClassInstance -class (get-scsmclass -name "microsoft.systemcenter.connector") | where {$_.displayname -like 'Azure Automation Connector'} | Remove-SCSMClassInstance
