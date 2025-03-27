namespace SweetMock.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using MemberBuilders;
using Microsoft.CodeAnalysis;
using SweetMock.Utils;

public class LogBuilder
{
    public static string InitializeLogging() =>
        $$"""
          # region Logging
          ->

          private bool _hasLog = false;
          private SweetMock.CallLog _log = new SweetMock.CallLog();
          
          /// <summary>
          /// Adding logging to the configuration.
          /// </summary>
          internal partial class Config
          {
            public Config LogCallsTo(SweetMock.CallLog callLog) {
                target._log = callLog;
                target._hasLog = true;
                return this;
            }
          }
          <-
          #endregion
          """;

    public static string BuildLogSegment(IMethodSymbol? symbol, bool skipParameters = false)
    {
        if (symbol == null) { return "";}

        string logSegment;
        if (!skipParameters && symbol.Parameters.Any(t => t.RefKind == RefKind.None))
        {
            var LogArgumentsValue = symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(argument);
            logSegment = $$"""
                           if(_hasLog) {
                           ->
                             _log.Add("{{symbol}}", SweetMock.Arguments{{string.Join("", LogArgumentsValue)}});
                           }
                           <-
                           """;
        }
        else
        {
            logSegment = $$"""
                           if(_hasLog) {
                           ->
                             _log.Add("{{symbol}}");
                           }
                           <-
                           """;
        }

        return logSegment;
    }

    private static string argument(IParameterSymbol t, int i)
    {
        return i == 0 ? $".With(\"{t.Name}\", {t.Name})" : $".And(\"{t.Name}\", {t.Name})";
    }
}
