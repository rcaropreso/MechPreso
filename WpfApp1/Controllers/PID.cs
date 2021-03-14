using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;
using WpfApp1.Utils;

namespace WpfApp1.Controllers
{
    class PID : IAsyncMessageUpdate
    {
        public float P { get; set; }
        public float I { get; set; }
        public float D { get; set; }
        public float SP { get; set; }
        public float TimeStep { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        private float _controlSignal;
        private float _PTerm = 0.0f;
        private float _ITerm = 0.0f;
        private float _DTerm = 0.0f;
        private float _previousError = 0.0f;
        private float _currentError = 0.0f;

        private bool DBG_MESSAGES_ON = false;

        #region Public Methods
        public PID()
        {
        }

        public PID(float _SP, float _P, float _I, float _D, float _TimeStep, 
            float _MinValue = 0, float _MaxValue = 1)
        {
            SP = _SP;
            P = _P;
            I = _I;
            D = _D;
            MinValue = _MinValue;
            MaxValue = _MaxValue;
            TimeStep = _TimeStep;
        }

        public float Output(float SysOutput)
        {
            UpdateError(SysOutput);
            return _controlSignal;
        }
        #endregion

        #region Private Methods
        //https://www.embarcados.com.br/controle-pid-em-sistemas-embarcados/

        //Para evitar o efeito de windup, vamos acrescentar o controle do valor acumulado do erro integral
        private void UpdateError(float sysOutput)
        {
            _previousError = _currentError;
            _currentError = SP - sysOutput;
            this._PTerm = _currentError * P;

            //# sysout -> saida do sistema monitorada pelo PID e comparada ao set_point
            StringBuilder sMessage = new StringBuilder();
            sMessage.AppendFormat("PID ERROR = {0}", _currentError);
            SendMessage(sMessage.ToString());

            // Erro integral - cumulativo
            this._ITerm += I * (_currentError * TimeStep); //termo integral

            //windup control
            this._ITerm = Math.Min(_ITerm, MaxValue);
            this._ITerm = Math.Max(_ITerm, MinValue);

            //Erro diferencial
            this._DTerm = D * ((_currentError - _previousError) / TimeStep);           

            //Saida do PID atualizada
            _controlSignal = _PTerm + _ITerm + _DTerm;

            sMessage.Clear();
            sMessage.AppendFormat("PID PTerm = {0} - ITerm = {1} - DTerm = {2} - Current Error = {3}", _PTerm, _ITerm, _DTerm, _currentError);
            SendMessage(sMessage.ToString());

            var cSig = _controlSignal;

            _controlSignal = Math.Min(_controlSignal, MaxValue);
            _controlSignal = Math.Max(_controlSignal, MinValue);

            sMessage.Clear();
            sMessage.AppendFormat("PID CONTROL SIG = {0} - Saturated = {1}", cSig, _controlSignal);
            SendMessage(sMessage.ToString());
        }

        public void SendMessage(string strMessage)
        {
            if (DBG_MESSAGES_ON)
            {
                Console.WriteLine(strMessage);
                Mediator.Notify("SendMessage", strMessage);
            }
        }
        #endregion
    }
}
