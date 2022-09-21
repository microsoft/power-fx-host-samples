import React, { useState } from 'react'
import { TabNavBar } from './TabNavBar'
import { TokenTab } from './TokenTab';
import { TabDictionary } from '../types';
import { EvaluationTab } from './EvaluationTab'
import { ParseTab } from './ParseTab';

export const Tabs = ({ expression, evaluation, tokens, parse }) => {
  const [activeTab, setActiveTab] = useState(1);



  return (
    <div>
      <TabNavBar TabDictionary={TabDictionary} setActiveTab={setActiveTab} />
      { activeTab === TabDictionary['evaluation'].id ? <EvaluationTab evaluation={evaluation} />: null }
      { activeTab === TabDictionary['tokens'].id ? <TokenTab tokens={tokens} expression={expression} />: null }
      { activeTab === TabDictionary['parse'].id ? <ParseTab parse={parse} /> : null }
    </div>
  )
}
