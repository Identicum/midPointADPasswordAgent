# midPoint Active Directory live password agent.

## Functional description

This ~~application~~ PoC listens to AD password change requests and synchronizes the changes with midPoint.

## Components

- ADPasswordFilter.dll
- ADPasswordAgent.exe

## Technical description

ADPasswordFilter.dll runs in the context of an AD Domain Controller and listens for AD password change requests.
ADPasswordAgent.exe synchronizes passwords passed by ADPasswordFilter.dll with midPoint.

## ToDo
- [ ] If midPoint service is down, ADPasswordAgent must group the password changes and synchronize the changes once the services becomes available.

