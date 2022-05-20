import React, { Component } from 'react';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
    this.state = { forecasts: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  static renderForecastsTable(forecasts) {
    return (
      <table border="1">        
        <tbody>
          {forecasts.map(forecast =>
            <tr key={forecast.key}>
              <td>{forecast.key}</td>
              <td>{forecast.value}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderForecastsTable(this.state.forecasts);

    return (
      <div>
        <p><b>Version Info</b></p>
        {contents}
      </div>
    );
  }

  async populateWeatherData() {
    const response = await fetch('VersionInfo');
    const data = await response.json();
    this.setState({ forecasts: data, loading: false });
  }
}
