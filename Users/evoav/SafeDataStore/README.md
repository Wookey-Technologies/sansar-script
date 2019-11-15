# Safe Data Store

In order to easily share table keys between scripts between different scripters we need a standard way use a secret for all our table keys, so that for every table a user wants to use they have to supply a secret, but that secret should be used the same in every script along with the table name in the way it forms the key, and it would need a foolproof way to have standard entropy including avoiding default values for the secret. 

The mechanism I propose is have reflectives in the scene for shared "database" secrets, then every other script just needs to know the database name and table name where each database had its secret configured. Hiding database names from other scripts is also needed.

You could do this with just a single int param and a table name but 4 billion possible numbers is not enough entropy, plus it would lead to default numbers (11111, 2222, etc) being used. We could use a range parameter so that its easier to pick a random number with the mouse, but there is not enough granularity when dragging the slider. So i suggest a reflective that has several range parameters that exceed int limits when strung together.
