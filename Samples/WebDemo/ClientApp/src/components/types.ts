export interface TabListItem {
    id: number;
    title: string;
    isActive: boolean;
}

export interface TabNavigationProp {
    tabList: TabListItem[];
}

export interface TabProp {
    tabList: TabListItem[];
    result: string;
}

export interface Span {
    lim: number;
    min: number;
}

export interface Token {
    kind: number;
    span: Span;
}

export const TabDictionary = {
    'evaluation': {
        id: 1,
        title: 'Evaluation'
    },
    'tokens': {
        id: 2,
        title: 'Tokens'
    },
    'parse': {
        id: 3,
        title: 'Parse'
    }
}

export const TokenKindList = [
    'None',
    'Eof',
    'Error',
    'Ident',
    'NumLit',
    'StrLit',
    'Comment',
    'Whitespace',
    'Add',
    'Sub',
    'Mul',
    'Div',
    'Caret',
    'ParenOpen',
    'ParenClose',
    'CurlyOpen',
    'CurlyClose',
    'BracketOpen',
    'BracketClose',
    'Equ',
    'Lss',
    'LssEqu',
    'Grt',
    'GrtEqu',
    'LssGrt',
    'Comma',
    'Dot',
    'Colon',
    'Ampersand',
    'PercentSign',
    'Semicolon',
    'At',
    'Or',
    'And',
    'Bang',
    'True',
    'False',
    'In',
    'Exactin',
    'Self',
    'Parent',
    'KeyOr',
    'KeyAnd',
    'KeyNot',
    'As',
    'StrInterpStart',
    'StrInterpEnd',
    'IslandStart',
    'IslandEnd'
]