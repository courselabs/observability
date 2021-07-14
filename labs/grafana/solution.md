
- infra panel:

sum without(job, tier) (up{tier="backend"})

- web panel:

sum without(job, tier) (up{tier="web"})

- infra panel:

sum without(job, tier) (up{tier="infrastructure"})