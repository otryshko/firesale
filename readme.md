Usage:
1. Install prereqs: `brew install dotnet-sdk`, `dotnet tool install --global dotnet-ef`
2. Clone repo, cd to repo folder.
3. Install postgres.
4. Edit file Infra/ApplicationDbContext.cs with the correct connection string.
5. Run `dotnet ef database update --project Infra` to init the DB.
6. Run `dotnet test Test` to run the test suite.
7. To see UI, run `dotnet run --project Api` and open `http://localhost:5079/graphql`


Design details:
Initial purchases are started with an in-memory dictionaries to track the user/item states.
Only completed purchases are saved in the DB with serializable transaction isolation level.
There is a tread which runs on timer and cancels non-completed purchases which have been created more than 30 seconds ago.
I didn't implement refund - should be quite straightforward, with an in-memory lock + transaction.
There is no auth, user ids are passed in.

Initially I considered doing everything with Postgres, but then having serializable transactions contesting the same row would create a lot of retries.
Having the state in memory is a bit dangerous in case the host goes down, but it can be addressed with live probes and secondaries.
I'm quite confident it'll meed 5K req/s - I did an ad-hoc stress testing not included in the repo and saw it doing 15KR/sec on my laptop.

