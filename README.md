# Premises

For my solution, I assumed that all requests to PowerService could be used in a UTC format as it is the final output.
The challenge states that the report MUST be available at the scheduled time, so my assumption would be to trigger an alert based on Error logs to identify the problem as soon as it happens.

# Improvements

- Add configCat to make our system parameters changeable from a friendly enpoint that won't require a deploy to update them;
- Add integration test to validate the retry policies;
- Deploy logs into a Monitoring tool such as NewRelic/Splunk so warnings and errors could be alerted.