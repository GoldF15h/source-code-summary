using System;

namespace Phahung_BN.Models
{
    public class ResponseModel
    {
        public string message;

        public int status;

        public ResponseModel(string _message, int _status)
        {
            message = _message;
            status = _status;
        }
        
    }
}
