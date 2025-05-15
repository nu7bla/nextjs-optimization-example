using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class StudentDashboard : Form
    {
        private int _userId;
        private string _connectionString = @"Your SQL Server connection string here";

        public StudentDashboard(int userId)
        {
            _userId = userId;
            InitializeComponent();
            LoadEnrolledSubjectsAndTeachers();
        }

        private void LoadEnrolledSubjectsAndTeachers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT s.SubjectID, s.SubjectName, t.TeacherID, t.FullName AS TeacherName
                        FROM Students st
                        INNER JOIN Enrollments e ON st.StudentID = e.StudentID
                        INNER JOIN Subjects s ON e.SubjectID = s.SubjectID
                        INNER JOIN SubjectTeachers stch ON s.SubjectID = stch.SubjectID
                        INNER JOIN Teachers t ON stch.TeacherID = t.TeacherID
                        WHERE st.UserID = @UserID";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserID", _userId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Bind to a DataGridView or ListView for display
                    dgvSubjectsTeachers.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading subjects and teachers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGiveFeedback_Click(object sender, EventArgs e)
        {
            if (dgvSubjectsTeachers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a teacher to give feedback.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teacherId = Convert.ToInt32(dgvSubjectsTeachers.SelectedRows[0].Cells["TeacherID"].Value);
            FeedbackForm feedbackForm = new FeedbackForm(_userId, teacherId);
            feedbackForm.ShowDialog();
        }
    }
}
