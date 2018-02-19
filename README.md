# JobMon
The Jobs Monitor processes emails from various job boards including JobServe. The filtering provided by job boards is not great - either the filters are too lax and the emails contain too many job ads to read, or the filters are too strict and you get hardly any job ads. JobMon allows you to set up much more criteria: jobs which pass the JobMon set of filtering criteria are stored in a database. When the user reviews jobs in the database, criteria can be updated.

JobMon is a personal project designed to run in the environment on my computer. It has a long history and has been through several generations. The first generation was written in Perl: it processed each email, parsed the jobs, transcibed the interesting jobs to a separate text file, and finally opened the file using the default application for text files. Whenever I collected emails, several Windows Notepad instances would open up on my desktop. The current generation is written using WinForms C#. Jobs and filters are held in a Sqlite database. I use Eudora (yes I know it is old!) as my email client to collect jobs. A redeeming feature of Eudora is that it is easy to set up filters to apply various operations to emails that match the filter criteria - in my Eudora filters, those emails are saved as text files to a folder where JobMon is watching! JobMon springs into action, parses the text files, and keeps the user updated with the number of "unread jobs" in the database. The user can then run a report to read those jobs, or to identify previous jobs.

There are a number of upgrades that I would like to do - if only I had time. WinForms is old - the software should be WPF or web-based. It is ugly requiring an email client that will helpfully deposit job emails as text into a folder - an RSS connection to the job site would be better (assuming that the job site supports RSS). Unfortunately (I do not know how to get around this one), each supported job site requires coding to help JobMon parse the job emails, and every so often, the job sites change the format of their emails (or even use several different styles interchangeably within their job emails) - it becomes necessary to revise the code when the format of the emails is changed.

## Rev_1_039
1. Bug-fix: when scrolling down through job summary and then stepping onto the next job, the job summary was remaining at that scroll position. Now ensure that top of job summary is displayed when stepping between jobs.
2. To cater for recovery from faulty RSS feeds, there is now a delay of 1 hour between retries.
3. Bug-fix: the count of new jobs on the main form did not take into account any RSS jobs just processed.

## Rev_1_038:
Rework to the reporting UI. I found the navigation buttons at the bottom of the form problematic when I made the form fill the height of the screen and I had the Windows taskbar on autohide - too easy to make the taskbar pop up when I wanted to click a button. The buttons are now in a single panel on the form.

## Rev_1_037:
Parse the salary and location for JobServe RSS jobs (in Rev_1_036, RSS jobs do not show salary or location).

## Rev_1_036:
Implementation of RSS reader alternative to job emails. These are configured in the JobMon settings.

## Rev_1_035:
Port to Visual Studio 2015 Community Edition.
First version on GitHub.
See the Changelog.txt file for history prior to entry in Git.

