using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StudentFeedbackSystem
{
    public partial class FeedbackForm : Form
    {
        private int _studentUserId;
        private int _teacherId;
        private string _connectionString = @"Your SQL Server connection string here";

        public FeedbackForm(int studentUserId, int teacherId)
        {
            _studentUserId = studentUserId;
            _teacherId = teacherId;
            InitializeComponent();
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            // For simplicity, hardcode 10 questions here
            string[] questions = new string[]
            {
                "Clarity of explanation",
                "Knowledge of subject",
                "Engagement with students",
                "Punctuality",
                "Use of teaching aids",
                "Encouragement of participation",
                "Feedback on assignments",
                "Approachability",
                "Organization of material",
                "Overall teaching effectiveness"
            };

            for (int i = 0; i < questions.Length; i++)
            {
                Label lbl = new Label();
                lbl.Text = $"{i + 1}. {questions[i]}";
                lbl.AutoSize = true;
                lbl.Top = 30 * i + 10;
                lbl.Left = 10;
                this.Controls.Add(lbl);

                ComboBox cb = new ComboBox();
                cb.Name = $"cbQuestion{i + 1}";
                cb.Top = lbl.Top - 3;
                cb.Left = 350;
                cb.Width = 50;
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
                cb.SelectedIndex = 4; // default 5
                this.Controls.Add(cb);
            }

            // Add comment box
            Label commentLabel = new Label();
            commentLabel.Text = "Optional anonymous comment:";
            commentLabel.Top = 320;
            commentLabel.Left = 10;
            commentLabel.AutoSize = true;
            this.Controls.Add(commentLabel);

            TextBox txtComment = new TextBox();
            txtComment.Name = "txtComment";
            txtComment.Top = 350;
            txtComment.Left = 10;
            txtComment.Width = 400;
            txtComment.Height = 80;
            txtComment.Multiline = true;
            this.Controls.Add(txtComment);

            // Add submit button
            Button btnSubmit = new Button();
            btnSubmit.Text = "Submit Feedback";
            btnSubmit.Top = 440;
            btnSubmit.Left = 10;
            btnSubmit.Click += BtnSubmit_Click;
            this.Controls.Add(btnSubmit);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            // Validate all questions answered
            int[] ratings = new int[10];
            for (int i = 1; i <= 10; i++)
            {
                ComboBox cb = (ComboBox)this.Controls[$"cbQuestion{i}"];
                if (cb.SelectedItem == null)
                {
                    MessageBox.Show($"Please select a rating for question {i}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ratings[i - 1] = int.Parse(cb.SelectedItem.ToString());
            }

            TextBox txtComment = (TextBox)this.Controls["txtComment"];
            string comment = txtComment.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Get StudentID from UserID
                    SqlCommand cmdStudent = new SqlCommand("SELECT StudentID FROM Students WHERE UserID = @UserID", conn);
                    cmdStudent.Parameters.AddWithValue("@UserID", _studentUserId);
                    int studentId = (int)cmdStudent.ExecuteScalar();

                    // Check if feedback already exists
                    SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Feedback WHERE StudentID = @StudentID AND TeacherID = @TeacherID", conn);
                    cmdCheck.Parameters.AddWithValue("@StudentID", studentId);
                    cmdCheck.Parameters.AddWithValue("@TeacherID", _teacherId);
                    int count = (int)cmdCheck.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("You have already submitted feedback for this teacher.", "Duplicate Feedback", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Insert into Feedback
                    SqlCommand cmdInsertFeedback = new SqlCommand("INSERT INTO Feedback (StudentID, TeacherID) OUTPUT INSERTED.FeedbackID VALUES (@StudentID, @TeacherID)", conn);
                    cmdInsertFeedback.Parameters.AddWithValue("@StudentID", studentId);
                    cmdInsertFeedback.Parameters.AddWithValue("@TeacherID", _teacherId);
                    int feedbackId = (int)cmdInsertFeedback.ExecuteScalar();

                    // Insert answers
                    for (int i = 0; i < 10; i++)
                    {
                        SqlCommand cmdInsertAnswer = new SqlCommand("INSERT INTO FeedbackAnswers (FeedbackID, QuestionNumber, Rating) VALUES (@FeedbackID, @QuestionNumber, @Rating)", conn);
                        cmdInsertAnswer.Parameters.AddWithValue("@FeedbackID", feedbackId);
                        cmdInsertAnswer.Parameters.AddWithValue("@QuestionNumber", i + 1);
                        cmdInsertAnswer.Parameters.AddWithValue("@Rating", ratings[i]);
                        cmdInsertAnswer.ExecuteNonQuery();
                    }

                    // Insert comment if any
                    if (!string.IsNullOrEmpty(comment))
                    {
                        SqlCommand cmdInsertComment = new SqlCommand("INSERT INTO FeedbackComments (FeedbackID, CommentText) VALUES (@FeedbackID, @CommentText)", conn);
                        cmdInsertComment.Parameters.AddWithValue("@FeedbackID", feedbackId);
                        cmdInsertComment.Parameters.AddWithValue("@CommentText", comment);
                        cmdInsertComment.ExecuteNonQuery();
                    }

                    MessageBox.Show("Thank you for your feedback!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting feedback: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
