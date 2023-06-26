using MetroFramework.Forms;
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

namespace Univap.Programacao3.Engine.Engine
{
    public class UserControlEngine : UserControl
    {
        protected void UpdateFields<T>(T content)
        {
            foreach (dynamic c in this.Controls)
            {
                if ((c is TextBox || c is MaskedTextBox || c is ComboBox)
                    && c.Tag != null)
                {
                    /// :: Add value to the object.
                    string[] controlName = c.Name.ToString().Split('_');
                    if (controlName.Count() != 3)
                        continue;
                    else if (controlName[1] == typeof(T).Name.ToString())
                    {
                        PropertyInfo pinfo = typeof(T).GetProperty(controlName[2]);
                        c.Text = Convert.ToString(pinfo.GetValue(content, null));
                    }
                }
                else if (c is CheckBox)
                {
                    /// :: Add value to the object.
                    string controlName = c.Name.ToString().Split('_')[2];
                    PropertyInfo pinfo = typeof(T).GetProperty(controlName);
                    c.Checked = Convert.ToInt32(pinfo.GetValue(content, null)) == 1 ? true : false;
                }
            }
        }

        protected void ApplyMaks()
        {
            foreach (dynamic c in this.Controls)
            {
                if ((c is TextBox || c is MaskedTextBox || c is ComboBox)
                    && c.Tag != null)
                {
                    string[] options = options = c.Tag.ToString().Trim().Split(';'); ;

                    if (options.Contains("required"))
                    {
                        int x = c.Location.X;
                        int y = c.Location.Y;
                        int h = c.Size.Height;

                        var newLabel = new Label();
                        newLabel.Location = new Point(x - 1, y + 1);
                        newLabel.Text = "|";
                        newLabel.ForeColor = Color.Red;
                        newLabel.BackColor = Color.Red;
                        newLabel.AutoSize = false;
                        newLabel.Size = new Size(2, h - 2);
                        //newLabel.Font = new Font("Arial", 8, FontStyle.Bold);

                        Controls.Add(newLabel);
                    }
                    if (options.Contains("onlynumbers"))
                        c.KeyPress += new KeyPressEventHandler(OnlyNumber_KeyPress);
                }

            }
        }

        /// <summary>
        /// Check if fields.
        /// </summary>
        /// <param name="this">UserControl</param>
        /// <returns>boollian</returns>
        protected PlayValidateField<T> ValidateFields<T>() where T : new()
        {
            /// :: Base.
            ValidateFieldEnum.Result flagResult = ValidateFieldEnum.Result.Ok;
            var checkResult = new PlayValidateField<T>()
            {
                Status = ValidateFieldEnum.Result.Ok,
                Content = new T()
            };

            foreach (dynamic c in this.Controls)
            {
                if (c is TextBox || c is MaskedTextBox || c is ComboBox)
                {
                    string[] controlName = c.Name.ToString().Split('_');
                    if (controlName.Count() != 3)
                        flagResult = ValidateFieldEnum.Result.WrongFormat;
                    else if (controlName[1] == typeof(T).Name.ToString())
                    {
                        checkResult = IsFieldOk<T>(c, checkResult.Content);
                        flagResult = flagResult != ValidateFieldEnum.Result.Ok ? flagResult : checkResult.Status;
                    }
                }
                else if (c is CheckBox)
                {
                    string[] controlName = c.Name.ToString().Split('_');
                    if (controlName.Count() != 3)
                        flagResult = ValidateFieldEnum.Result.WrongFormat;
                    else if (controlName[1] == typeof(T).Name.ToString())
                    {
                        /// :: Add value to the object.
                        int content = c.Checked ? 1 : 0;
                        PropertyInfo pinfo = typeof(T).GetProperty(controlName[2]);
                        pinfo.SetValue(checkResult.Content, Convert.ChangeType(content, pinfo.PropertyType), null);
                        flagResult = ValidateFieldEnum.Result.Ok;
                    }
                }
            }

            checkResult.Status = flagResult;
            return checkResult;
        }

        /// <summary>
        /// Clean all fiels.
        /// </summary>
        /// <param name="this">UserControl</param>
        protected void CleanFields()
        {
            foreach (var c in this.Controls)
            {
                if (c is TextBox)
                    ((TextBox)c).Text = String.Empty;
                if (c is MaskedTextBox)
                    ((MaskedTextBox)c).Text = String.Empty;
            }
        }

        private void OnlyNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != ',')
                e.Handled = true;
        }

        /// <summary>
        /// Check if field is ok.
        /// </summary>
        /// <typeparam name="T">Class that will return velue (attributes name has to be the same of the components.)</typeparam>
        /// <param name="box">Component object.</param>
        /// <param name="content">Object that will receive components content.</param>
        /// <returns>Return status and object content.</returns>
        private PlayValidateField<T> IsFieldOk<T>(dynamic box, T content)
        {
            /// :: Base.
            box.Text = box.Text.Trim();
            string info = Regex.Replace(box.Text.Trim(), @"[-./,;\s+]", "");
            string[] options = null;
            bool hasErrror = false;

            /// :: Check observations.
            if (box.Tag != null)
            {
                options = box.Tag.ToString().Trim().Split(';');
                hasErrror = options.Contains("required") && String.IsNullOrEmpty(info)
                    || options.Contains("cpf") && !CPFIsOk(info);
            }

            /// :: Check if is empity.
            if (hasErrror)
            {
                box.BackColor = ColorTranslator.FromHtml("#ffe2e2");
                return new PlayValidateField<T>()
                {
                    Status = ValidateFieldEnum.Result.FieldEmpity,
                    Content = content
                };
            }

            /// :: Add value to the object.
            string controlName = box.Name.ToString().Split('_')[2];
            PropertyInfo pinfo = typeof(T).GetProperty(controlName);
            string[] tags = box.Tag.ToString().Trim().Split(';');

            if (pinfo.PropertyType == typeof(DateTime))
            {
                pinfo.SetValue(content, Convert.ChangeType(
                    Convert.ToDateTime(box.Text),
                    pinfo.PropertyType), null);
            }
            else if((box is ComboBox) && options.Contains("index"))
                pinfo.SetValue(content, Convert.ChangeType(box.SelectedIndex, pinfo.PropertyType), null);
            else
                pinfo.SetValue(content, Convert.ChangeType(box.Text, pinfo.PropertyType), null);

            box.BackColor = ColorTranslator.FromHtml("#ffffff");

            return new PlayValidateField<T>()
            {
                Status = ValidateFieldEnum.Result.Ok,
                Content = content
            };
        }


        private static bool CPFIsOk(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}
