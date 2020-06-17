# SimuEngine
A project to create a not-very-versatile and somewhat well-structured game engine. Hopefully.

## Engine Design
The engine is made from three separate parts.
1. The player object
1. The system
1. The handler

The player object is the interface between the player and the engine. It is able to do two things. First, it can view information from the system. Secondly, it can perform "actions", as defined by the game, which influences
the system in ways defined by the action. Actions have two components, requirements and consequences. Requirements need to be fulfilled before they can use the action. Consequences is what the action does.

The system is an information graph, with each node having the ability to contain a subgraph. Nodes also have traits, numerical fields; statuses, informational tags; connections, identifiers that reference other nodes; a type,
an identifier that tells the system what the node is; and a group list, a list of groups the node belongs to. The system also handles the creation of itself, along with checking for possible and required events. Groups are
lists of people that also have traits and statuses. Connections also have traits and statuses. Events are autonomous actions.

The handler checks for flagged events and triggers all required ones, then decides whether or not to execute them.