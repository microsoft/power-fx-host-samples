import React from 'react'
import { TokenKindList } from '../types';

export const TokenTab = ({ tokens, expression }) => {
    return (
        <div>
            <h2> Tokens </h2>
            <table style={{maxHeight:'100px', overflow:"auto"}}>
                <tbody >
                    <tr>
                        <th>Token Kind</th>
                        <th>Token Span</th>
                        <th>Token Value</th>
                    </tr>
                    {
                        tokens.map(
                            (token) => {
                                return (
                                    <tr> 
                                        <td>{ TokenKindList[token.kind]}</td>
                                        <td>[{token.span.min} - {token.span.lim})</td>
                                        <td>{expression.substring(token.span.min, token.span.lim)}</td>
                                    </tr>
                                );
                            }
                        )
                    }
                </tbody>
            </table>
        </div>
    )
}

export default TokenTab