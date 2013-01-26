using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace TestService
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        int Add(int x,int y);

        [OperationContract]
        int Substract(int x, int y);

        [OperationContract]
        string Hello(string target);
    }
}
