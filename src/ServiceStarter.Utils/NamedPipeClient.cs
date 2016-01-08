using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CStarter.Utils
{
    public abstract class NamedPipeClient<T>
    {
        private T _Proxy;

        private ChannelFactory<T> _Factory;

        public NamedPipeClient()
        {
            _Proxy = default(T);
        }

        protected virtual void InitializeClient(EndpointAddress ep, Action success, Action<string> failure)
        {
            try
            {
                _Factory = new ChannelFactory<T>(new NetNamedPipeBinding(), ep);

                _Proxy = _Factory.CreateChannel();

                success();
            }
            catch (Exception eX)
            {
                eX.Exception();
                failure(eX.Message);
            }
        }

        protected virtual void ExecuteNamedPipeAction(EndpointAddress ep, Action<T> executeAction, int retryTimes, Action<string> failure)
        {
            bool finished = false;
            string failureMsg = "";

            while(retryTimes > 0 && !finished)
            {
                InitializeClient(ep, () =>
                {
                    try
                    {
                        executeAction(_Proxy);
                        _Factory.Close();
                        finished = true;
                    }
                    catch (CommunicationException eX)
                    {
                        eX.Exception();
                        _Factory.Abort();
                        retryTimes--;
                        failureMsg = eX.Message;
                    }
                    catch (TimeoutException eX)
                    {
                        eX.Exception();
                        _Factory.Abort();
                        retryTimes--;
                        failureMsg = eX.Message;
                    }
                    catch (Exception eX)
                    {
                        _Factory.Abort();
                        eX.Exception();
                        retryTimes--;
                        failureMsg = eX.Message;
                    }
                }, (msg) =>
                {
                    failureMsg = msg;
                    retryTimes--;
                });
            }

            if(!finished)
            {
                failure(failureMsg);
            }
        }

        protected void ExecuteNamedPipeAction(EndpointAddress ep, Action<T> executeAction, Action<string> failure)
        {
            ExecuteNamedPipeAction(ep, executeAction, 1, failure);
        }
    }
}
