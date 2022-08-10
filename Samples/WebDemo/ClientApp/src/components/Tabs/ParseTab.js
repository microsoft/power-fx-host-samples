import React from 'react'

export const ParseTab = ({ parse }) => {
    return (
        <div>
            <h2> Parse Tree </h2>
            <pre>
                {parse}
            </pre>
        </div>
    )
}
