namespace Back.Entities ;
public class StudentGrade // تسميها StudentGrade عشان في enum اسمه Grade
{
    public int Id { set; get; }
    public decimal Score { set; get; }
    public string GradeType { set; get; } // Midterm, Final, etc
    public DateTime DateGiven { set; get; }

    public int StudentId { set; get; }
    public Student Student { set; get; }

    public int TeacherId { set; get; }
    public Teacher Teacher { set; get; }

    public int SubjectId { set; get; }
    public Subject Subject { set; get; }
}
