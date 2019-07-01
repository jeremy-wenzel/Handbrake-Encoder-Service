namespace HandBrakeEncoder
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TestService = new System.ServiceProcess.ServiceProcessInstaller();
            this.MyTestService = new System.ServiceProcess.ServiceInstaller();
            // 
            // TestService
            // 
            this.TestService.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.TestService.Password = null;
            this.TestService.Username = null;
            this.TestService.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.TestService_AfterInstall);
            // 
            // MyTestService
            // 
            this.MyTestService.Description = "My First Test Service";
            this.MyTestService.DisplayName = "MyTestService";
            this.MyTestService.ServiceName = "Service1";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.TestService,
            this.MyTestService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller TestService;
        private System.ServiceProcess.ServiceInstaller MyTestService;
    }
}