using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Univap.Programacao3.Engine.Model;

namespace Univap.Programacao3.Engine.Engine.Interface
{
    public interface IFormEngine
    {
        void UpdateFields<T>(T content);
        void ApplyMaks();
        PlayValidateField<T> ValidateFields<T>();
        void CleanFields();
        void OnlyNumber_KeyPress(object sender, KeyPressEventArgs e);
        PlayValidateField<T> IsFieldOk<T>(dynamic box, T content);
        bool CPFIsOk(string cpf);
    }
}
