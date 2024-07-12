namespace questions.Models
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string QuestionAnswer { get; set; }
        public bool IsMandatory { get; set; }
    }
}
