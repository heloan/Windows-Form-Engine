namespace Univap.Programacao3.Engine.Model
{
    public class PlayValidateField<T>
    {
        public ValidateFieldEnum.Result Status { get; set; }
        public T Content { get; set; }
    }
}
