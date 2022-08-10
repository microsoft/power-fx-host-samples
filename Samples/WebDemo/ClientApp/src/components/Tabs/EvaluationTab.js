import React from 'react'

export const EvaluationTab = ({ evaluation: evaluation }) => {
    return (
        <div>
            <h2> Evaluation </h2>
                <textarea style={
                    {
                        width: 'calc(100% - 6px)',
                        height: 100,
                        border: "1px solid grey"
                    }
                }
                    value={evaluation}
                    readOnly={true} />
        </div>
    )
}
