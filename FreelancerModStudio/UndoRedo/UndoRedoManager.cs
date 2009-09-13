// This source is under LGPL license. Sergei Arhipenko (c) 2006-2007. email: sbs-arhipenko@yandex.ru. This notice may not be removed.
using System;
using System.Collections.Generic;
using System.Text;
//TODO: 
// m-threads (Join?)
// invisible commands

namespace FreelancerModStudio
{
	/// <summary>
	/// This facade class provides a simplified undo/redo functionality.
	/// Use this class if you do not need multiple undo/redo areas with isolation.
	/// </summary>
    public static class UndoRedoManager
    {
		public static readonly UndoRedoArea DefaultArea = new UndoRedoArea("#Default");

		static UndoRedoManager()
		{
			DefaultArea.CommandDone += delegate(object sender, CommandDoneEventArgs type)
			{
				if (CommandDone != null)
					CommandDone(sender, type);
			};
		}
		/// <summary>Returns true if history has command that can be undone</summary>
        public static bool CanUndo
        {
            get { return DefaultArea.CanUndo;	}
        }
		/// <summary>Returns true if history has command that can be redone</summary>
        public static bool CanRedo
        {
            get { return DefaultArea.CanRedo;	}
        }
		/// <summary>Undo last command from history list</summary>
        public static void Undo()
        {
			DefaultArea.Undo();
        }
		/// <summary>
		/// Repeats command that was undone before
		/// </summary>
        public static void Redo()
        {
			DefaultArea.Redo();
        }
		/// <summary>Start command. Any data changes must be done within a command.</summary>
		/// <param name="commandCaption"></param>
		/// <returns>Interface that allows properly finish the command with 'using' statement</returns>
        public static IDisposable Start(string commandCaption)
        {
			return DefaultArea.Start(commandCaption);
        }
		/// <summary>
		/// Start a command with affinity checking. 
		/// If several commands with equal captions and owners follow each other,
		/// they are affined and will be merged into single command.
		/// This method is useful when you want a bunch of similar routine actions looks like a single command. 
		/// E.g. user moves a rectangle 10 times and then sees one Move command in the undo list.
		/// </summary>
		/// <param name="commandCaption">Caption of the command</param>
		/// <param name="owner">
		/// Owner is used as an identifier to check affinity of commands. Any object can be an owner. 
		/// If command has no owner (null), it never has affinity with any other command.
		/// </param>
		/// <returns>Interface that allows properly finish the command with 'using' statement</returns>
		public static IDisposable Start(string commandCaption, object owner)
		{
			return DefaultArea.Start(commandCaption, owner);
		}
		/// <summary>
		/// Start invisible command. 
		/// Any data changes must be done within a command. 
		/// This command will never appear in the history. 
		/// It will be undone/redone in bundle with previous visible command.</summary>
		/// <param name="commandCaption"></param>
		/// <returns>Interface that allows properly finish the command with 'using' statement</returns>
		/// <remarks><para>
		/// Invisible commands are useful if you need to do some changes by some event 
		/// but do not expose them to user as a standalone command. </para>
		/// <para>For example, when user clicks on object, we could change SelectedObject property.
		/// However, it is redundant to show this operation in history and allow to undo/redo it as a valuable command.
		/// Instead of that, we can start invisible command and its results will be joined to previous command. 
		/// Thus, when the previuos command will be undone, the selection will be undone too.
		/// </remarks>
		public static IDisposable StartInvisible(string commandCaption)
		{
			return DefaultArea.StartInvisible(commandCaption);
		}
		/// <summary>Commits current command and saves changes into history</summary>
        public static void Commit()
        {
			DefaultArea.Commit();
        }
		/// <summary>
		/// Rollback current command. It does not saves any changes done in current command.
		/// </summary>
        public static void Cancel()
        {
			DefaultArea.Cancel();
        }
		[Obsolete("This method was substituted with ClearHistory method (you must close command before invocation)",true)]
        public static void FlushHistory()
        {
        }
		/// <summary>
		/// Clears all history. It does not affect current data but history only. 
		/// It is usefull after any data initialization if you want forbid user to undo this initialization.
		/// </summary>
		public static void ClearHistory()
		{
			DefaultArea.ClearHistory();
		}
		/// <summary>Gets an enumeration of commands captions that can be undone.</summary>
		/// <remarks>The first command in the enumeration will be undone first</remarks>
		public static IEnumerable<string> UndoCommands
		{
			get
			{
				return DefaultArea.UndoCommands;
			}
		}
		/// <summary>Gets an enumeration of commands captions that can be redone.</summary>
		/// <remarks>The first command in the enumeration will be redone first</remarks>
		public static IEnumerable<string> RedoCommands
		{
			get
			{
				return DefaultArea.RedoCommands;
			}
		}

		public static event EventHandler<CommandDoneEventArgs> CommandDone;

        /// <summary>
        /// Gets/sets max commands stored in history. 
        /// Zero value (default) sets unlimited history size.
        /// </summary>
        public static int MaxHistorySize
        {
            get { return DefaultArea.MaxHistorySize; }
			set { DefaultArea.MaxHistorySize = value; }
        }

		public static bool IsCommandStarted
		{
			get { return DefaultArea.IsCommandStarted; }
		}
	}

	public enum CommandDoneType
	{
		Commit, Undo, Redo
	}

	public class CommandDoneEventArgs : EventArgs
	{ 
		public readonly CommandDoneType CommandDoneType;
		public CommandDoneEventArgs(CommandDoneType type)
		{
			CommandDoneType = type;
		}
	}

}
