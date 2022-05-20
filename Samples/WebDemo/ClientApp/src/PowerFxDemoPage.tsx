/*!
 * Copyright (C) Microsoft Corporation. All rights reserved.
 */

import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
import * as React from 'react';

import { IDisposable, MessageProcessor, PowerFxFormulaEditor } from '@microsoft/power-fx-formulabar/lib';

import { sendDataAsync } from './Helper';
import { PowerFxLanguageClient } from './PowerFxLanguageClient';
import { FetchData } from './components/FetchData';

interface PowerFxDemoPageState {
  context: string;    // additional symbols passed in as a json object. 
  expression: string; // the user's Power Fx expression to be evaluated 
  evaluation: string; // the string-ified result of the evaluation. 
  hasErrors: boolean;
}

export default class PowerFxDemoPage extends React.Component<{}, PowerFxDemoPageState> {
  private _languageClient: PowerFxLanguageClient;
  private _messageProcessor: MessageProcessor;
  private _editor: monaco.editor.ICodeEditor | undefined;
  private _listener: (data: string) => void = () => null;

  constructor(props: {}) {
    super(props);

    const onDataReceived = (data: string) => {
      this._listener(data);
    };

    this._languageClient = new PowerFxLanguageClient(onDataReceived);
    this._messageProcessor = {
      addListener: (listener: (data: string) => void): IDisposable => {
        this._listener = listener;
        return {
          dispose: () => null
        };
      },
      sendAsync: async (data: string): Promise<void> =>
        this._languageClient.sendAsync(data)
    };

    this.state = {
      context: JSON.stringify({ "A": "ABC", "B": { "Inner": 123 } }),
      expression: '',
      evaluation: '',
      hasErrors: false
    };
  }

  public render() {
    const { context, expression, evaluation, hasErrors } = this.state;
    return (
      <div>
        <h3>Context</h3>
        <p>This is a JSON object whose properties become 'globals' in the Power Fx expression below.</p>
        <textarea style={{
          width: 'calc(100% - 6px)',
          height: 100,
          border: "1px solid grey"
        }}
          value={context}
          onChange={(ev) => {
            const context = ev.target.value;
            this.setState({ context });
            this._evalAsync(context, expression);
          }} />

        <h3>Formula</h3>
        <p>This is a Power Fx expression</p>
        <PowerFxFormulaEditor
          getDocumentUriAsync={this._getDocumentUriAsync}
          defaultValue={''}
          messageProcessor={this._messageProcessor}
          maxLineCount={10}
          minLineCount={4}
          onChange={(newValue: string): void => {
            this.setState({ expression: newValue, hasErrors: false });
            this._evalAsync(context, newValue);
          }}
          onEditorDidMount={(editor, _): void => { this._editor = editor }}
          lspConfig={{
            enableSignatureHelpRequest: true
          }}
        />

        <h3>Evaluation Result</h3>
        <textarea style={{
          width: 'calc(100% - 6px)',
          height: 100,
          border: "1px solid grey"
        }}
          value={evaluation}
          readOnly={true} />

        <FetchData />
      </div>
    );
  }

  private _getDocumentUriAsync = async (): Promise<string> => {
    return `powerfx://demo?context=${this.state.context}`;
  };

  private _evalAsync = async (context: string, expression: string): Promise<void> => {
    const result = await sendDataAsync('eval', JSON.stringify({ context, expression }));
    if (!result.ok) {
      return;
    }

    const response = await result.json();
    if (response.result) {
      this.setState({ evaluation: response.result, hasErrors: false });
    } else if (response.error) {
      this.setState({ evaluation: response.error, hasErrors: true });
    } else {
      this.setState({ evaluation: '', hasErrors: false });
    }
  };
}
