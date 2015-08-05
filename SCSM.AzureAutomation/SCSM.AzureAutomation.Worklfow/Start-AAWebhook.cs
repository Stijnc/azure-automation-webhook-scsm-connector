using System; 
using System.ComponentModel; 
using System.ComponentModel.Design; 
using System.Workflow.ComponentModel.Design; 
using System.Workflow.ComponentModel; 
using System.Workflow.ComponentModel.Serialization; 
using System.Workflow.ComponentModel.Compiler; 
using System.Collections; 
using System.Workflow.Activities; 
using System.Workflow.Runtime; 


namespace SCSM.AzureAutomation.Worklfow
{
    public partial class Start_AAWebhookActivity : System.Workflow.Activities.SequentialWorkflowActivity
    {
        // this is a dummy Workflow service manager needs. All logic is contained in the MP (when using a PowerShell script, otherwise service manager is perfectly capable to handle WWF)
    }
}
