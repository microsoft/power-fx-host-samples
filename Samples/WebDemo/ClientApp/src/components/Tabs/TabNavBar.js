import { object } from 'prop-types'
import React from 'react'

export const TabNavBar = ({ TabDictionary, setActiveTab }) => {
    return (
        <div>
            {
                Object.keys(TabDictionary).map(
                    (key, value) => (
                        <button onClick={ () => setActiveTab(TabDictionary[key].id)} key = {TabDictionary[key].id} >
                            {TabDictionary[key].title}
                        </button>  
                    )
                )
            }
        </div>
    )
}
