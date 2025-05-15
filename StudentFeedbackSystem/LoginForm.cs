using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string loginCode = txtLoginCode.Text.Trim();

            if (string.IsNullOrEmpty(loginCode))
            {
                MessageBox.Show("Please enter your login code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate login code against database
            if (ValidateLoginCode(loginCode, out string role, out int userId))
            {
                // Open appropriate dashboard based on role
                this.Hide();
                if (role == "student")
                {
                    StudentDashboard studentDashboard = new StudentDashboard(userId);
                    studentDashboard.ShowDialog();
                }
                else if (role == "teacher")
                {
                    TeacherDashboard teacherDashboard = new TeacherDashboard(userId);
                    teacherDashboard.ShowDialog();
                }
                else if (role == "admin")
                {
                    AdminDashboard adminDashboard = new AdminDashboard(userId);
                    adminDashboard.ShowDialog();
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid login code.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateLoginCode(string loginCode, out string role, out int userId)
        {
            role = null;
            userId = -1;

            string connectionString = @"Your SQL Server connection string here";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, Role FROM Users WHERE LoginCode = @LoginCode";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LoginCode", loginCode);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                        role = reader.GetString(1);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }
    }
}
