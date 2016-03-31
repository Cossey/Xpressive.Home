﻿using log4net;
using Xpressive.Home.Contracts.Messaging;

namespace Xpressive.Home.Messaging
{
    internal class MessageQueueLogListener :
        IMessageQueueListener<UpdateVariableMessage>,
        IMessageQueueListener<CommandMessage>,
        IMessageQueueListener<NotifyUserMessage>
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (MessageQueueLogListener));

        public void Notify(UpdateVariableMessage message)
        {
            _log.Info($"{message.GetType().Name} for variable {message.Name} received.");
        }

        public void Notify(CommandMessage message)
        {
            _log.Info($"{message.GetType().Name} for action {message.ActionId} received.");
        }

        public void Notify(NotifyUserMessage message)
        {
            _log.Info($"{message.GetType().Name} received: {message.Notification}");
        }
    }
}