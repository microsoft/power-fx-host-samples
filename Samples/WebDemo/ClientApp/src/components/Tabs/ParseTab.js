import React from 'react'

export const ParseTab = ({ parse }) => {
    return (
        <div>
            <h2> Parse Tree </h2>
            <pre>
                { /* Need to JSON.parse() first, so later we can format it with stringify */ }
                {JSON.stringify(JSON.parse(parse), null, 4)}
            </pre>
        </div>
    )
}
