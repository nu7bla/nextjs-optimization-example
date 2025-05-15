using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class TeacherDashboard : Form
    {
        private int _userId;
        private string _connectionString = @"Your SQL Server connection string here";

        public TeacherDashboard(int userId)
        {
            _userId = userId;
            InitializeComponent();
            LoadFeedbackSummary();
        }

        private void LoadFeedbackSummary()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            q.QuestionNumber,
                            AVG(CAST(fa.Rating AS FLOAT)) AS AverageRating
                        FROM FeedbackAnswers fa
                        INNER JOIN Feedback f ON fa.FeedbackID = f.FeedbackID
                        INNER JOIN Teachers t ON f.TeacherID = t.TeacherID
                        INNER JOIN Users u ON t.UserID = u.UserID
                        INNER JOIN (
                            SELECT DISTINCT QuestionNumber FROM FeedbackAnswers
                        ) q ON fa.QuestionNumber = q.QuestionNumber
                        WHERE u.UserID = @UserID
                        GROUP BY q.QuestionNumber
                        ORDER BY q.QuestionNumber";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserID", _userId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvAverageRatings.DataSource = dt;

                    // Load comments
                    string commentQuery = @"
                        SELECT fc.CommentText, f.SubmittedAt
                        FROM FeedbackComments fc
                        INNER JOIN Feedback f ON fc.FeedbackID = f.FeedbackID
                        INNER JOIN Teachers t ON f.TeacherID = t.TeacherID
                        INNER JOIN Users u ON t.UserID = u.UserID
                        WHERE u.UserID = @UserID
                        ORDER BY f.SubmittedAt DESC";

                    SqlCommand cmdComments = new SqlCommand(commentQuery, conn);
                    cmdComments.Parameters.AddWithValue("@UserID", _userId);

                    SqlDataAdapter adapterComments = new SqlDataAdapter(cmdComments);
                    DataTable dtComments = new DataTable();
                    adapterComments.Fill(dtComments);

                    dgvComments.DataSource = dtComments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading feedback summary: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
