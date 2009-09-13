// This source is under LGPL license. Sergei Arhipenko (c) 2006-2007. email: sbs-arhipenko@yandex.ru. This notice may not be removed.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FreelancerModStudio
{
    class Command : IDisposable
    {
		readonly UndoRedoArea parentArea;
        public readonly string Caption;
		internal readonly bool Visible;
		Dictionary<IUndoRedoMember, object> changes = new Dictionary<IUndoRedoMember, object>();

		public Command(string caption, UndoRedoArea parentArea, bool visible)
        {
            Caption = caption;
			this.parentArea = parentArea;
			this.Visible = visible;
        }

		public bool IsEnlisted(IUndoRedoMember member)
		{
			// if command suspended, it will always return true to prevent changes registration
			return changes.ContainsKey(member);
		}

		public object this[IUndoRedoMember member]
		{
			get 
			{
				return changes[member];
			}
			set 
			{
				changes[member] = value;
			}
			
		}

		internal void Commit()
		{
			foreach (IUndoRedoMember member in changes.Keys)
				member.OnCommit(changes[member]);
		}
		internal void Undo()
		{
			if (merges != null)
				foreach (IUndoRedoMember member in merges.Keys)
					member.OnUndo(merges[member]);

			foreach (IUndoRedoMember member in changes.Keys)
				member.OnUndo(changes[member]);
		}
		internal void Redo()
		{
			foreach (IUndoRedoMember member in changes.Keys)
				member.OnRedo(changes[member]);

			if (merges != null)
				foreach (IUndoRedoMember member in merges.Keys)
					member.OnRedo(merges[member]);
		}

		public bool HasChanges
		{
			get { return changes.Count > 0; }
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			if (parentArea.IsCommandStarted)
			{
				Debug.Assert(parentArea.CurrentCommand == this, "Another command had been started within disposed command");
				parentArea.Cancel();
			}
		}

		#endregion

		Dictionary<IUndoRedoMember, object> merges;
		internal void Merge(Command mergedCommand)
		{
			if (merges == null)
				merges = new Dictionary<IUndoRedoMember, object>();
			foreach (IUndoRedoMember member in mergedCommand.changes.Keys)
				merges[member] = mergedCommand[member];
		}
	}
}
