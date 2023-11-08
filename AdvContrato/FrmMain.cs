using AdvContratoDomain.Entities;

namespace AdvContrato
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            UserManager manager = new UserManager(@"mongodb+srv://AdminAmaral:82Dt2NdQCBvIkTH7@genfile.el4ckfk.mongodb.net/?retryWrites=true&w=majority", "Validacao");

            var users = await manager.GetAllUsersAsync();
            string email = "ga-amaral@live.com";

            var key = users.Find(u=>u.Email == email).Key;

            var test = await manager.AuthenticateUserAsync(email, key);



        }
    }
}