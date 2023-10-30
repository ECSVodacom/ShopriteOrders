namespace ShopriteClaimsDownloadService
{
    partial class ShopriteClaimsProjectInstaller
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
            this.ShopriteClaimsserviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ShopriteClaimsserviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ShopriteClaimsserviceProcessInstaller
            // 
            this.ShopriteClaimsserviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.ShopriteClaimsserviceProcessInstaller.Password = null;
            this.ShopriteClaimsserviceProcessInstaller.Username = null;
            // 
            // ShopriteClaimsserviceInstaller
            // 
            this.ShopriteClaimsserviceInstaller.Description = "Downloads Claims for Shoprite and inserts the XML file into the database";
            this.ShopriteClaimsserviceInstaller.DisplayName = "Shoprite Claims Downloader";
            this.ShopriteClaimsserviceInstaller.ServiceName = "ShopriteClaimsDownloadService";
            // 
            // ShopriteClaimsProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ShopriteClaimsserviceProcessInstaller,
            this.ShopriteClaimsserviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ShopriteClaimsserviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ShopriteClaimsserviceInstaller;
    }
}