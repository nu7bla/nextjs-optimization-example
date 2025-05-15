using System;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class AdminDashboard : Form
    {
        private int _userId;

        public AdminDashboard(int userId)
        {
            _userId = userId;
            InitializeComponent();
            // Admin functionalities like managing users, subjects, assignments, and reports can be added here
        }
    }
}
