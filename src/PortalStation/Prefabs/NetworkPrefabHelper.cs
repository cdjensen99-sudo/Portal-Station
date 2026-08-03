using System;
using UnityEngine;

namespace PortalStation;

internal static class NetworkPrefabHelper
{
    internal static void RunWithoutZdoCreation(Action action)
    {
        bool previous = ZNetView.m_forceDisableInit;
        ZNetView.m_forceDisableInit = true;
        try
        {
            action();
        }
        finally
        {
            ZNetView.m_forceDisableInit = previous;
        }
    }

    internal static T RunWithoutZdoCreation<T>(Func<T> factory)
    {
        bool previous = ZNetView.m_forceDisableInit;
        ZNetView.m_forceDisableInit = true;
        try
        {
            return factory();
        }
        finally
        {
            ZNetView.m_forceDisableInit = previous;
        }
    }
}
